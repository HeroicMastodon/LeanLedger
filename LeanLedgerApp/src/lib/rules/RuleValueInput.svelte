<script lang="ts">
    import {debounce, type RuleTransactionField, ruleTransactionFields} from "$lib/rules/index";
    import {transactionTypeOptions} from "$lib/transactions";
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import type {SelectOption} from "$lib";
    import PredictiveSelect from "$lib/components/forms/PredictiveSelect.svelte";

    let {field, value = $bindable(), disabled = false, dataListId, accounts, onLoadTextPredictions, selectPopupName}: {
        field: RuleTransactionField;
        value?: string;
        disabled?: boolean;
        dataListId: string;
        accounts: SelectOption<string>[];
        onLoadTextPredictions: (value: string) => Promise<any>;
        selectPopupName: string;
    } = $props();
</script>

{#if field === "Type"}
    <select disabled={disabled} bind:value class="select">
        {#each transactionTypeOptions as field}
            <option value={field.value}>{field.display}</option>
        {/each}
    </select>
{:else if field === "Date"}
    <input disabled={disabled} class="input" type="date" bind:value />
{:else if field === "Amount" && value}
    <MoneyInput disabled={disabled} bind:value />
{:else if field === "Source" || field === "Destination"}
    <PredictiveSelect
        popupTargetName={selectPopupName}
        options={accounts}
        optional
        bind:value
        disabled={disabled}
    />
{:else}
    <input
        class="input"
        type="text"
        list={dataListId}
        bind:value
        disabled={disabled}
        onkeyup={debounce((e: {target: HTMLInputElement}) => onLoadTextPredictions(e.target.value))}
    />
{/if}
