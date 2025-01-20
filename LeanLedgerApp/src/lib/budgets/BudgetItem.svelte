<script lang="ts">

    import {formatMoney, type MaybePromise} from "$lib";
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";

    let {expected = $bindable(), actual, name = $bindable(), nameIsEditable = true, barColor, onSave}: {
        expected: number;
        actual: number;
        name: string;
        nameIsEditable?: boolean;
        barColor: 'success' | 'error' | 'warning';
        onSave: () => MaybePromise<any>;
    } = $props();

    let mode = $state<"view" | "edit">("view");

    function edit() {
        mode = "edit";
    }

    async function save() {
        await onSave();
        mode = "view";
    }
</script>

<div class="flex flex-row gap-4 items-start">
    {#if mode === "view"}
        <h2 class="h2 mb-4">{name}</h2>
        <button onclick={edit} class="btn variant-outline-secondary">Edit</button>
    {:else}
        {#if nameIsEditable}
            <LabeledInput
                type="text"
                bind:value={name}
                label="Name"
            />
        {:else}
            <h2 class="h2 mb-4">{name}</h2>
        {/if}
        <button onclick={save} class="btn variant-outline-success">Save</button>
    {/if}
</div>
<div class="mb-4">
    <ProgressBar
        meter="bg-{barColor}-500"
        track="bg-{barColor}-500/30"
        min={0}
        max={expected}
        value={actual}
        height="h-4"
    />
</div>
{#if mode === "view"}
    <p class="p">{formatMoney(actual)} of {formatMoney(expected)}</p>
{:else}
    <div class="flex gap-4 items-center mb-4">
        <MoneyInput
            label="Expected"
            bind:value={expected}
        />
        <MoneyInput
            readonly
            label="Actual"
            value={actual}
        />
    </div>
{/if}
