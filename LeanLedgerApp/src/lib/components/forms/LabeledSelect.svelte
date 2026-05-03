<script lang="ts" generics="Option">
    import type { MaybePromise, SelectOption } from "$lib";

    let {
        value = $bindable(),
        label,
        class: className,
        options,
        optional = false,
        onchange,
        disabled = false,
    }: {
        value?: Option;
        optional?: boolean;
        label?: string;
        class?: string;
        options: SelectOption<Option>[];
        onchange?: (e: {
            currentTarget: HTMLSelectElement;
        }) => MaybePromise<void>;
        disabled?: boolean;
    } = $props();
</script>

<label class="label {className ?? ''}">
    {#if label}
        <span>{label}</span>
    {/if}
    <select class="select" bind:value {onchange} {disabled}>
        {#if optional}
            <option value={undefined}>(none)</option>
        {/if}
        {#each options as option}
            <option value={option.value}>{option.display}</option>
        {/each}
    </select>
</label>
