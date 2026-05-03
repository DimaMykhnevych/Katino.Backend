using Katino.Domain.Enums;
using Katino.Domain.Models.Pnl;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using Katino.Domain.Repositories.FinanceEntryRepository;
using Katino.Domain.Services.FinanceEntryN.GenerateFinancePnlReport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.FinanceEntryN;

public class FinancePnlReportService : IFinancePnlReportService
{
    private readonly IFinanceEntryRepository _financeEntryRepository;
    private readonly IFinanceCategoryRepository _financeCategoryRepository;
    private readonly ILogger _logger;

    public FinancePnlReportService(
        IFinanceEntryRepository financeEntryRepository,
        IFinanceCategoryRepository financeCategoryRepository,
        ILoggerFactory loggerFactory)
    {
        _financeEntryRepository = financeEntryRepository;
        _financeCategoryRepository = financeCategoryRepository;
        _logger = loggerFactory?.CreateLogger(nameof(FinancePnlReportService));
    }

    public async Task<PnlReport> BuildAsync(int year, CancellationToken ct)
    {
        _logger.LogInformation($"Building PnL report for {year} year");

        _logger.LogDebug("Getting expense categories");
        var expenseCategories = await _financeCategoryRepository.Query()
            .AsNoTracking()
            .Where(x => x.Type == FinanceCategoryType.Expense && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync(ct);

        _logger.LogDebug("Calculating income aggregates");
        var incomeAgg = await _financeEntryRepository.Query()
            .AsNoTracking()
            .Where(e => e.EntryDate.Year == year)
            .Where(e => e.Category.Type == FinanceCategoryType.Income)
            .GroupBy(e => new { e.EntryDate.Month, e.SaleType })
            .Select(g => new
            {
                g.Key.Month,
                g.Key.SaleType,
                Sum = g.Sum(x => x.Amount)
            })
            .ToListAsync(ct);

        _logger.LogDebug("Calculating expense aggregates");
        var expenseAgg = await _financeEntryRepository.Query()
            .AsNoTracking()
            .Where(e => e.EntryDate.Year == year)
            .Where(e => e.Category.Type == FinanceCategoryType.Expense)
            .GroupBy(e => new { e.EntryDate.Month, e.CategoryId })
            .Select(g => new
            {
                g.Key.Month,
                g.Key.CategoryId,
                Sum = g.Sum(x => x.Amount)
            })
            .ToListAsync(ct);

        decimal[] revenueRetail = new decimal[12];
        decimal[] revenueDropWholesale = new decimal[12];

        foreach (var x in incomeAgg)
        {
            if (x.SaleType == SaleType.Retail)
            {
                AddToMonth(revenueRetail, x.Month, x.Sum);
            }
            else if (x.SaleType == SaleType.Drop || x.SaleType == SaleType.Wholesale)
            {
                AddToMonth(revenueDropWholesale, x.Month, x.Sum);
            }
            else
            {
                AddToMonth(revenueDropWholesale, x.Month, x.Sum);
            }
        }

        _logger.LogDebug("Calculating total revenue");
        decimal[] revenueTotal = new decimal[12];
        for (int i = 0; i < 12; i++)
        {
            revenueTotal[i] = revenueRetail[i] + revenueDropWholesale[i];
        }

        var expenseByCategory = expenseCategories.ToDictionary(
            c => c.Id,
            c => new decimal[12]);

        foreach (var x in expenseAgg)
        {
            if (!expenseByCategory.TryGetValue(x.CategoryId, out var months))
            {
                _logger.LogWarning($"Category {x.CategoryId} is deleted or is inactove, skipping it from report");
                continue;
            }

            AddToMonth(months, x.Month, x.Sum);
        }

        _logger.LogDebug("Calculating total expenses");
        decimal[] expensesTotal = new decimal[12];
        foreach (var kv in expenseByCategory)
            for (int i = 0; i < 12; i++)
                expensesTotal[i] += kv.Value[i];

        _logger.LogDebug("Calculating margin and margin percentage");
        decimal[] margin = new decimal[12];
        decimal[] marginPct = new decimal[12];
        for (int i = 0; i < 12; i++)
        {
            margin[i] = revenueTotal[i] - expensesTotal[i];
            marginPct[i] = revenueTotal[i] == 0m ? 0m : Math.Round((margin[i] / revenueTotal[i]) * 100m, 2);
        }

        _logger.LogDebug("Building PnL rows");
        var rows = new List<PnlRow>();

        var rowRevenueTotal = MakeRow("rev.total", "Revenue total", PnlRowKind.RevenueTotal, revenueTotal);
        var rowRevenueRetail = MakeRow("rev.retail", "Retail", PnlRowKind.RevenueRetail, revenueRetail);
        var rowRevenueDrop = MakeRow("rev.drop", "Drop", PnlRowKind.RevenueDropWholesale, revenueDropWholesale);

        rows.Add(rowRevenueTotal);
        rows.Add(rowRevenueRetail);
        rows.Add(rowRevenueDrop);

        var rowExpensesTotal = MakeRow("exp.total", "Expenses Total", PnlRowKind.ExpensesTotal, expensesTotal);
        rows.Add(rowExpensesTotal);

        foreach (var cat in expenseCategories)
        {
            var months = expenseByCategory[cat.Id];
            rows.Add(MakeRow($"exp.cat.{cat.Id}", cat.Name, PnlRowKind.ExpenseCategory, months));
        }

        var rowMargin = MakeRow("marg.value", "Margin", PnlRowKind.Margin, margin);
        rows.Add(rowMargin);

        rows.Add(new PnlRow
        {
            Key = "marg.pct",
            Title = "Margin %",
            Kind = PnlRowKind.MarginPercent,
            Months = marginPct.ToArray(),
            Total = revenueTotal.Sum() == 0m ? 0m : Math.Round((margin.Sum() / revenueTotal.Sum()) * 100m, 2),
            SharePercent = null
        });

        var totalRevenueYear = rowRevenueTotal.Total;
        var totalExpensesYear = rowExpensesTotal.Total;

        _logger.LogDebug("Calculating share percentage");
        foreach (var r in rows)
        {
            if (r.Kind is PnlRowKind.RevenueTotal or PnlRowKind.RevenueRetail or PnlRowKind.RevenueDropWholesale)
            {
                r.SharePercent = totalRevenueYear == 0m ? 0m : Math.Round((r.Total / totalRevenueYear) * 100m, 2);
            }
            else if (r.Kind is PnlRowKind.ExpensesTotal or PnlRowKind.ExpenseCategory)
            {
                r.SharePercent = totalExpensesYear == 0m ? 0m : Math.Round((r.Total / totalExpensesYear) * 100m, 2);
            }
            else
            {
                r.SharePercent = null;
            }
        }

        _logger.LogInformation($"PnL report for {year} year is ready! Sending to client");
        return new PnlReport { Year = year, Rows = rows };
    }

    private static void AddToMonth(decimal[] months, int month1to12, decimal value)
    {
        var idx = month1to12 - 1;
        if (idx >= 0 && idx < 12)
        {
            months[idx] += value;
        }
    }

    private static PnlRow MakeRow(string key, string title, PnlRowKind kind, decimal[] months)
    {
        return new PnlRow
        {
            Key = key,
            Title = title,
            Kind = kind,
            Months = months.ToArray(),
            Total = months.Sum()
        };
    }
}
