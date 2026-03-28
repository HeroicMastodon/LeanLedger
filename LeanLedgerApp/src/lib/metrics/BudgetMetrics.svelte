<script lang="ts">
    import { apiClient } from "$lib/apiClient";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import { ProgressBar } from "@skeletonlabs/skeleton";
    import Card from "$lib/components/Card.svelte";
    import Money from "$lib/components/Money.svelte";
    import type { AxiosError } from "axios";
    import { type TransactionType } from "$lib/transactions";

    type BudgetMetric = {
        budgeted: number;
        actual: number;
        difference: number;
    };

    type BudgetMetrics = {
        expectedIncome: number;
        expectedExpenses: number;
        totalExpenses: number;
        leftToSpend: number;

        income: BudgetMetric;
        expenses: BudgetMetric;
        change: BudgetMetric;
    };

    const metricsPromise = $derived.by(async () => {
        const resp = await apiClient.get<BudgetMetrics>(
            `metrics/budget?${monthManager.params}`,
        );
        return resp.data;
    });

    function getError(err: AxiosError<string>) {
        return err.response?.data ?? err.message ?? "Something went wrong";
    }
</script>

{#snippet metricRow(metric: BudgetMetric, type?: TransactionType)}
    <td><Money {type} amount={metric.budgeted} /></td>
    <td><Money {type} amount={metric.actual} /></td>
    <td><Money {type} amount={metric.difference} /></td>
{/snippet}

{#await metricsPromise}
    <Card>
        <h2 class="h2">Budget</h2>
        <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
    </Card>
{:then metrics}
    <Card>
        <h2 class="h2">Budget</h2>
        <hr class="hr mb-4" />
        <table class="w-96">
            <thead>
                <tr>
                    <th></th>
                    <th class="text-left">Budgeted</th>
                    <th class="text-left">Actual</th>
                    <th class="text-left">Difference</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>Income</td>
                    {@render metricRow(metrics.income)}
                </tr>
                <tr>
                    <td>Expenses</td>
                    {@render metricRow(metrics.expenses, "Expense")}
                </tr>
                <tr>
                    <td>Change</td>
                    {@render metricRow(metrics.change)}
                </tr>
            </tbody>
        </table>
    </Card>
{:catch err}
    <Card class="variant-filled-error">
        <h2 class="h2">Budget</h2>
        <hr class="hr mb-4" />
        <p class="p">Could not retrieve budget metrics</p>
        <p class="p">{getError(err)}</p>
    </Card>
{/await}
