<script lang="ts">
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";
    import type {Transaction} from "$lib/transactions";
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";

    let transactions: Transaction[] = $state([]);

    async function load() {
        const response = await apiClient.get<{ transactions: Transaction[] }>("transactions");
        transactions = response.data.transactions;
    }
</script>

<h1 class="h1 mb-8">Transactions</h1>
<TransactionTable transactions={transactions} />
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:catch err}
    <div class="alert variant-filled-error">
        <div class="alert-message">
            <h3 class="h3">Something went wrong</h3>
            <p>We couldn't load your transactions. Please try again </p>
            <p>{!!err ? err : ""}</p>
        </div>
    </div>
{/await}
