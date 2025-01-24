<script lang="ts">
    import Card from "$lib/components/Card.svelte";
    import {apiClient} from "$lib/apiClient";
    import {monthManager} from "$lib/selectedMonth.svelte";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import Money from "$lib/components/Money.svelte";
    type NetWorth = {
            netWorth: number;
            income: number;
            expenses: number;
        };

    const netWorthPromise = $derived.by(async () => {
        const resp = await apiClient.get<NetWorth>(`Metrics/net-worth?${monthManager.params}`);

        return resp.data;
    });
</script>


{#await netWorthPromise}
    <Card>
        <h2 class="h2">Net Worth</h2>
        <hr class="hr" />
        <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
    </Card>
{:then netWorth}
    <Card>
        <h2 class="h2">Net Worth: <Money  amount={netWorth.netWorth} /></h2>
        <hr class="hr mb-4" />
        <div class="flex gap-4">
            <div>Expenses: <Money amount={netWorth.expenses} type="Expense"/></div>
            <div>Income: <Money amount= {netWorth.income} /> </div>
            <div>Change: <Money amount={netWorth.income - netWorth.expenses} /> </div>
        </div>
    </Card>
{/await}
