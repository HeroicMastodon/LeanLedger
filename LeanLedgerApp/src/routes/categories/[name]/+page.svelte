<script lang="ts">
    import Money from "$lib/components/Money.svelte";
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import type {Transaction} from "$lib/transactions";
    import type {PageData} from "./$types";
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";

    type Category = { amount: number; name: string; transactions: Transaction[] }

    const {data}: { data: PageData } = $props();
    let category: Category = $state({amount: 0, name: "", transactions: []});

    async function load() {
        const resp = await apiClient.get<Category>(`categories/${data.name}`);
        category = resp.data;
    }
</script>

<h1 class="h1 mb-8"> {data.name} - Total: <Money amount={category.amount}/></h1>
<TransactionTable
    transactions={category.transactions}
/>
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{/await}
