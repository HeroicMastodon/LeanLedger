<script lang="ts">
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import {apiClient} from "$lib/apiClient";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import LabeledSelect from "$lib/components/forms/LabeledSelect.svelte";
    import type {SelectOption} from "$lib";
    import Card from "$lib/components/Card.svelte";

    type ImportSettings = {
        dateFormat: string;
        csvDelimiter: string;
        importMappings: {
            sourceColumnName: string;
            destinationField: TransactionField;
        }[]
    }
    const transactionFields = [
        "Ignore",
        "Amount",
        "NegatedAmount",
        "Date",
        "Description",
        "Category",
    ] as const;
    const transactionFiledOptions: SelectOption<TransactionField>[] = transactionFields.map(tf => ({
        display: tf,
        value: tf
    }));
    type TransactionField = typeof transactionFields[number];

    let {accountId}: { accountId: string; } = $props();

    let error = $state("");
    let settings = $state<ImportSettings>({dateFormat: "", csvDelimiter: ",", importMappings: []});

    async function onClick() {
        const res = await apiClient.get<ImportSettings>(`imports/settings/${accountId}`);
        settings = res.data;
    }

    async function onSave() {
        const res = await apiClient.put(`imports/settings/${accountId}`, settings);
        return true;
    }

    function removeMapping(idx: number) {
        settings.importMappings.splice(idx, 1);
    }

    function addMapping() {
        settings.importMappings.push({destinationField: "Ignore", sourceColumnName: ""})
    }
</script>
<FormButton
    text="Import Settings"
    confirmText="Save"
    bind:error
    onClick={onClick}
    onConfirm={onSave}
    class="variant-filled-secondary"
>
    <div class="flex flex-col gap-4">
        <div class="flex flex-row gap-4">
            <LabeledInput
                label="Date Format"
                type="text"
                placeholder="YYYY-DD-MMMM"
                bind:value={settings.dateFormat}
            />
            <LabeledInput
                label="CSV Delimiter"
                type="text"
                bind:value={settings.csvDelimiter}
            />
        </div>
        <Card class="flex flex-col gap-4">
            <div class="flex flex-row gap-4 justify-between">
                <h3 class="h3">Mappings</h3>
                <button
                    class="btn variant-outline-primary"
                    onclick={addMapping}
                >Add
                </button>
            </div>
            {#each settings.importMappings as mapping, idx}
                <div class="flex flex-row gap-4 items-end">
                    <LabeledInput
                        label="Source Column"
                        type="text"
                        bind:value={mapping.sourceColumnName}
                    />
                    <LabeledSelect
                        label="Destination Field"
                        options={transactionFiledOptions}
                        bind:value={mapping.destinationField}
                    />
                    <button
                        class="btn variant-outline-error"
                        onclick={() => removeMapping(idx)}
                    >Delete
                    </button>
                </div>
            {/each}
        </Card>
    </div>
</FormButton>
