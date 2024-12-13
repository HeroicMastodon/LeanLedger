<script lang="ts">
    import {
        type EditableTransaction,
        loadAccountOptions,
        loadCategoryOptions,
        TransactionTypeOptions
    } from "$lib/transactions/index.js";
    import PredictiveText from "$lib/components/PredictiveText.svelte";
    import LabeledSelect from "$lib/components/LabeledSelect.svelte";
    import LabeledInput from "$lib/components/LabeledInput.svelte";
    import MoneyInput from "$lib/components/MoneyInput.svelte";
    import PredictiveSelect from "$lib/components/PredictiveSelect.svelte";
    import type {SelectOption} from "$lib";
    import {onMount} from "svelte";

    let {transaction = $bindable<EditableTransaction>()} = $props();
    let categories: string[] = $state([]);
    let accounts: SelectOption<string>[] = $state([]);
    onMount(async () => {
        categories = await loadCategoryOptions();
        accounts = await loadAccountOptions();
    })

</script>

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
        options={categories}
        class="md:col-span-2"
    />
    <PredictiveSelect
        class="md:col-span-3"
        label="Source Account"
        popupTargetName="source-account"
        options={accounts}
        optional
        bind:value={transaction.sourceAccountId}
    />
    <PredictiveSelect
        class="md:col-span-3"
        label="Destination Account"
        popupTargetName="destination-account"
        options={accounts}
        optional
        bind:value={transaction.destinationAccountId}
    />
</div>
