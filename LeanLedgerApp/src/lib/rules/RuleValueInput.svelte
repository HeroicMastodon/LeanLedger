<script lang="ts">
    import { debounce, type RuleTransactionField } from "$lib/rules/index";
    import { transactionTypeOptions } from "$lib/transactions";
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import type { SelectOption } from "$lib";
    import PredictiveSelect from "$lib/components/forms/PredictiveSelect.svelte";
    import LabeledSelect from "$lib/components/forms/LabeledSelect.svelte";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";

    let {
        field,
        value = $bindable(),
        disabled = false,
        dataListId: datalistId,
        accounts,
        onLoadTextPredictions,
        selectPopupName,
        label,
    }: {
        field: RuleTransactionField;
        value?: string;
        disabled?: boolean;
        dataListId: string;
        accounts: SelectOption<string>[];
        onLoadTextPredictions: (value: string) => Promise<any>;
        selectPopupName: string;
        label?: string;
    } = $props();

    const loadTextPredictions = debounce(
        (e: { target: HTMLInputElement }) =>
            onLoadTextPredictions(e.target.value),
        200,
    );
</script>

{#if field === "Type"}
    <LabeledSelect
        {label}
        options={transactionTypeOptions}
        bind:value
        {disabled}
    />
{:else if field === "Date"}
    <LabeledInput {label} {disabled} type="date" bind:value />
{:else if field === "Amount" && value}
    <MoneyInput {label} {disabled} bind:value />
{:else if field === "Source" || field === "Destination"}
    <PredictiveSelect
        {label}
        popupTargetName={selectPopupName}
        options={accounts}
        optional
        bind:value
        {disabled}
    />
{:else}
    <LabeledInput
        {label}
        type="text"
        list={datalistId}
        bind:value
        {disabled}
        onkeyup={loadTextPredictions}
    />
{/if}
