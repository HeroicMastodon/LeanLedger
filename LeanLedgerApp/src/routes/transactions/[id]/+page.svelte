<script lang="ts">
    import LabeledInput from "$lib/components/LabeledInput.svelte";
    import {
        type AttachedAccount,
        defaultTransaction, type EditableTransaction, loadAccountOptions, loadCategoryOptions,
        type Transaction,
        TransactionTypeOptions
    } from "$lib/transactions";
    import MoneyInput from "$lib/components/MoneyInput.svelte";
    import PredictiveText from "$lib/components/PredictiveText.svelte";
    import LabeledSelect from "$lib/components/LabeledSelect.svelte";
    import {onMount} from "svelte";
    import {apiClient} from "$lib/apiClient";
    import type {PageData} from "./$types";
    import type {SelectOption} from "$lib";
    import {type PopupSettings, popup} from "@skeletonlabs/skeleton";
    import PredictiveSelect from "$lib/components/PredictiveSelect.svelte";

    const popupSettings: PopupSettings = {
        event: 'focus-click',
        target: 'select',
        placement: 'bottom',
    };

    const {data}: { data: PageData; } = $props();

    let transaction: EditableTransaction = $state(defaultTransaction());

    $effect(() => {
        console.dir($state.snapshot(transaction))
        console.dir($state.snapshot(transaction.type))
    })

    let categories: string[] = $state([]);
    let accounts: SelectOption<string>[] = $state([]);
    onMount(async () => {
        const res = await apiClient.get<EditableTransaction>(`transactions/${data.id}`)
        console.dir(res.data);
        transaction = res.data;
        categories = await loadCategoryOptions();
        accounts = await loadAccountOptions();
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
