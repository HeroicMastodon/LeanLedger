<script lang="ts">
    import LabeledInput from "$lib/components/LabeledInput.svelte";
    import {defaultTransaction, type Transaction, TransactionTypeOptions} from "$lib/transactions";
    import MoneyInput from "$lib/components/MoneyInput.svelte";
    import PredictiveText from "$lib/components/PredictiveText.svelte";
    import LabeledSelect from "$lib/components/LabeledSelect.svelte";
    import {onMount} from "svelte";
    import {apiClient} from "$lib/apiClient";
    import type {PageData} from "./$types";

    const {data}: {data: PageData;} = $props();

    let transaction: Transaction = $state(defaultTransaction());
    // TODO: Load Categories
    // TODO: Load Accounts
    $effect(() => {
        console.dir($state.snapshot(transaction));
    })

    onMount(async () => {
        const res = await apiClient.get<Transaction>(`transactions/${data.id}`)
        transaction = res.data;
    })
</script>

<h1 class="h1 mb-8">Transaction</h1>

<div class="card p-8 grid grid-cols-1 md:grid-cols-6 gap-4 max-w-3xl items-center">
    <LabeledInput
        label="Description"
        type="text"
        bind:value={transaction.description}
        class="md:col-span-4"
    />
    <LabeledInput
        label="Date"
        type="date"
        bind:value={transaction.date}
        class="md:col-span-2"
    />
    <MoneyInput
        label="Amount"
        bind:value={transaction.amount}
        class="md:col-span-2"
    />
    <LabeledSelect
        bind:value={transaction.type}
        label="Type"
        options={TransactionTypeOptions}
        class="md:col-span-2"
    />
    <PredictiveText
        bind:value={transaction.category}
        inputId="category"
        datalistId="categories"
        label="Category"
        options={["Groceries", "Gas"]}
        class="md:col-span-2"
    />
    <LabeledSelect
        class="md:col-span-3"
        bind:value={transaction.sourceAccount}
        label="Source Account"
        optional
        options={[
            {
                display: "Savings",
                value: {
                    id: "savings",
                    name: "Savings"
                }
            },
            {
                display: "Checking",
                value: {
                    id: "checking",
                    name: "Checking"
                }
            },
        ]}
    />
    <LabeledSelect
        bind:value={transaction.destinationAccount}
        label="Destination Account"
        class="md:col-span-3"
        optional
        options={[
            {
                display: "Savings",
                value: {
                    id: "savings",
                    name: "Savings"
                }
            },
            {
                display: "Checking",
                value: {
                    id: "checking",
                    name: "Checking"
                }
            },
        ]}
    />
</div>
