<script lang="ts" generics="Option">
    import type {SelectOption} from "$lib";

    let {
        value = $bindable(),
        label,
        class: className,
        options,
        optional = false,
    }: {
        value?: Option;
        optional?: boolean;
        label: string;
        class?: string;
        options: SelectOption<Option>[];
    } = $props();
</script>

<label class="label {className ?? ''}">
    <span>{label}</span>
    <select class="select" bind:value={value}>
        {#if optional}
            <option value={undefined}>(none)</option>
        {/if}
        {#each options as option}
            <option value={option.value}>{option.display}</option>
        {/each}
    </select>
</label>
