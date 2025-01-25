<script lang="ts">
    import {apiClient} from "$lib/apiClient";
    import {monthManager} from "$lib/selectedMonth.svelte";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import Card from "$lib/components/Card.svelte";
    import Money from "$lib/components/Money.svelte";
    import type {AxiosError} from "axios";

    type BudgetMetrics = {
        expectedIncome: number;
        expectedExpenses: number;
        totalExpenses: number;
        leftToSpend: number;
    }

    const metricsPromise = $derived.by(async () => {
        const resp = await apiClient.get<BudgetMetrics>(`metrics/budget?${monthManager.params}`);
        return resp.data;
    });

    function getError(err: AxiosError<string>) {
        return err.response?.data ?? err.message ?? "Something went wrong";
    }
</script>

{#await metricsPromise}
    <Card>
        <h2 class="h2">Budget</h2>
        <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
    </Card>
{:then metrics}
    <Card>
        <h2 class="h2">Budget</h2>
        <hr class="hr mb-4" />
        <div class="grid grid-cols-2 gap-4">
            <div>Expected Expenses: <Money type="Expense" amount={metrics.expectedExpenses} /></div>
            <div>Expected Income: <Money amount={metrics.expectedIncome} /></div>
            <div>Total Expenses: <Money type="Expense" amount={metrics.totalExpenses} /> </div>
            <div>Left To Spend: <Money amount={metrics.leftToSpend} /> </div>
        </div>
    </Card>
    {:catch err}
    <Card class="variant-filled-error">
        <h2 class="h2">Budget</h2>
        <hr class="hr mb-4" />
        <p class="p">Could not retrieve budget metrics</p>
        <p class="p">{getError(err)}</p>
    </Card>
{/await}
