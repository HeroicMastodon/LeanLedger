<script lang="ts">

    import {formatMoney, type MaybePromise} from "$lib";
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import {Fa} from "svelte-fa";
    import {faEdit} from "@fortawesome/free-solid-svg-icons/faEdit";
    import {faSave} from "@fortawesome/free-solid-svg-icons/faSave";
    import PredictiveText from "$lib/components/forms/PredictiveText.svelte";

    let {
        expected = $bindable(),
        expectedIsEditable = true,
        actual,
        name = $bindable(),
        nameIsEditable = true,
        barColor,
        onSave,
        children,
        readonly,
        options,
        id,
        class: className
    }: {
        expected: number;
        expectedIsEditable?: boolean;
        actual: number;
        name: string;
        nameIsEditable?: boolean;
        barColor: 'success' | 'error' | 'warning';
        onSave?: () => MaybePromise<any>;
        children?: any;
        readonly?: boolean;
        options?: string[];
        id: string;
        class?: string;
    } = $props();

    let mode = $state<"view" | "edit">("view");

    function edit() {
        mode = "edit";
    }

    async function save() {
        await onSave?.();
        mode = "view";
    }
</script>

<div class={className}>
    <div
        class="flex flex-row mb-4 items-center"
        class:items-center={!nameIsEditable || readonly}
        class:items-end={mode === "edit" && nameIsEditable}
    >
        {#if mode === "view"}
            <h2 class="h2">{name}</h2>
            {#if !readonly}
                <button onclick={edit} class="btn btn-icon text-secondary-500">
                    <Fa icon={faEdit} />
                </button>
            {/if}
            {@render children?.()}
        {:else}
            {#if nameIsEditable}
                {#if options}
                    <PredictiveText
                        bind:value={name}
                        label="Name"
                        datalistId="budget-item-options-{id}"
                        inputId="budget-item-input-{id}"
                        options={options}
                    />
                {:else}
                    <LabeledInput
                        type="text"
                        bind:value={name}
                        label="Name"
                    />
                {/if}
            {:else}
                <h2 class="h2">{name}</h2>
            {/if}
            <button onclick={save} class="btn btn-icon text-success-500">
                <Fa icon={faSave} />
            </button>
        {/if}
    </div>
    <div class="mb-2">
        <ProgressBar
            meter="bg-{barColor}-500"
            track="bg-{barColor}-500/30"
            min={0}
            max={expected}
            value={actual}
            height="h-4"
        />
    </div>
    {#if mode === "view" || !expectedIsEditable}
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
</div>
