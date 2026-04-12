<script lang="ts">
    import { type SelectOption } from "$lib";
    import { apiClient } from "$lib/apiClient";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import PredictiveSelect from "$lib/components/forms/PredictiveSelect.svelte";
    import {  type PiggyBankEntry } from "$lib/piggybanks";

    let { entry = $bindable() }: { entry: PiggyBankEntry } = $props();
    let piggyBankOptions = $state<SelectOption<string>[]>([]);

    async function load() {
        const res = await apiClient.get<{id: string; name: string;}[]>("piggybanks/names");
        piggyBankOptions = res.data.map(pb => ({ value: pb.id, display: pb.name }));
    }

    load();
</script>

<div class="flex flex-col gap-4">
    <PredictiveSelect
        label="Piggy Bank"
        bind:value={entry.piggyBank.id}
        options={piggyBankOptions}
        popupTargetName="piggy-bank-select"
    />
    <LabeledInput
        type="text"
        label="Description"
        bind:value={entry.description}
    />
    <div class="flex gap-4">
        <LabeledInput type="number" label="Amount" bind:value={entry.amount} />
        <LabeledInput type="date" label="Date" bind:value={entry.date} />
    </div>

    <!-- TODO: Make this editable -->
    <!-- {#if entry.transaction?.id} -->
    <!--     <LabeledInput -->
    <!--         type="text" -->
    <!--         label="Transaction" -->
    <!--         value={entry.transaction.description} -->
    <!--         disabled -->
    <!--     /> -->
    <!-- {/if} -->
</div>
