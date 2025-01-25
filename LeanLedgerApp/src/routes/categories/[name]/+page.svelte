<script lang="ts">
    import Money from "$lib/components/Money.svelte";
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import type {Transaction} from "$lib/transactions";
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";
    import {page} from "$app/stores";
    import {monthManager} from "$lib/selectedMonth.svelte";

    type Category = { amount: number; name: string; transactions: Transaction[] }

    let category: Category = $state({amount: 0, name: "", transactions: []});
    let name = $page.params.name;

    async function load() {
        const resp = await apiClient.get<Category>(`categories/${name}?${monthManager.params}`);
        category = resp.data;
    }
</script>

<h1 class="h1 mb-8"> {name} - Total: <Money amount={category.amount}/></h1>
<TransactionTable
    transactions={category.transactions}
/>
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{/await}
