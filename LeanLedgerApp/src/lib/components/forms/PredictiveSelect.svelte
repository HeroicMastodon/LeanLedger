<script lang="ts" generics="OptionType">
    import {type PopupSettings, popup, ListBox, ListBoxItem} from "@skeletonlabs/skeleton";
    import type {SelectOption} from "$lib";

    let {
        value = $bindable(),
        options = $bindable(),
        popupTargetName,
        label,
        class: className,
        optional = false,
        disabled,
    }: {
        value: OptionType,
        options: SelectOption<OptionType>[];
        popupTargetName: string;
        label?: string;
        class?: string;
        optional?: boolean;
        disabled?: boolean;
    } = $props();

    const popupSettings: PopupSettings = {
        event: 'focus-blur',
        target: popupTargetName,
        placement: 'bottom',
        closeQuery: 'option'
    };

    let textValue = $state("");
    const filteredOptions = $derived(
        textValue
            ? options.filter(o => o.display.includes(textValue))
            : options
    );
    $effect(() => {
        textValue = options.filter(o => o.value == value)[0]?.display ?? '(none)';
    })

</script>

<label class="label {className ?? ''}">
    {#if label}
        <span>{label}</span>
    {/if}
    <input disabled={disabled} use:popup={popupSettings} class="input" type="text" bind:value={textValue} />
</label>
<select
    data-popup={popupTargetName}
    size={Math.max(Math.min(filteredOptions.length, 4), 2)}
    class="select max-w-fit"
    bind:value={value}
>
    {#if optional}
        <option value={undefined}>(none)</option>
    {/if}
    {#each filteredOptions as option}
        <option value={option.value}>{option.display}</option>
    {/each}
</select>
