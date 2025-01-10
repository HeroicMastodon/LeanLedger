<script lang="ts">
    import {debounce, type RuleTransactionField, ruleTransactionFields} from "$lib/rules/index";
    import {type AttachedAccount, transactionTypeOptions} from "$lib/transactions";
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";

    let {field, value = $bindable(), disabled = false, dataListId, accounts, onLoadTextPredictions}: {
        field: RuleTransactionField;
        value?: string;
        disabled?: boolean;
        dataListId: string;
        accounts: AttachedAccount[];
        onLoadTextPredictions: (value: string) => Promise<any>;
    } = $props();
</script>

{#if field === "Type"}
    <select bind:value class="select">
        {#each transactionTypeOptions as field}
            <option value={field.value}>{field.display}</option>
        {/each}
    </select>
{:else if field === "Date"}
    <input class="input" type="date" bind:value />
{:else if field === "Amount" && value}
    <MoneyInput bind:value />
{:else if field === "Source" || field === "Destination"}
    <select class="select" bind:value>
        {#each accounts as account}
            <option value={account.id}>{account.name}</option>
        {/each}
    </select>
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
