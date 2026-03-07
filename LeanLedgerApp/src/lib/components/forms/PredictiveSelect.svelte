<script lang="ts" generics="OptionType">
    import {
        type PopupSettings,
        popup,
        ListBox,
        ListBoxItem,
    } from "@skeletonlabs/skeleton";
    import type { SelectOption } from "$lib";

    let {
        value = $bindable(),
        options = $bindable(),
        popupTargetName,
        label,
        class: className,
        optional = false,
        disabled,
    }: {
        value: OptionType;
        options: SelectOption<OptionType>[];
        popupTargetName: string;
        label?: string;
        class?: string;
        optional?: boolean;
        disabled?: boolean;
    } = $props();

    const popupSettings: PopupSettings = {
        event: "focus-blur",
        target: popupTargetName,
        placement: "bottom",
        closeQuery: "option",
    };

    let textValue = $state("");
    const filteredOptions = $derived(
        textValue
            ? options.filter((o) =>
                  o.display.toLowerCase().includes(textValue.toLowerCase()),
              )
            : options,
    );
    $effect(() => {
        textValue =
            options.filter((o) => {
                if (typeof o.value === "string" && typeof value === "string") {
                    return o.value.toLowerCase() === value.toLowerCase();
                }

                return o.value == value;
            })[0]?.display ?? "(none)";
    });

    const selectSize = $derived(
        Math.max(Math.min(filteredOptions.length + 1, 4), 2),
    );
</script>

<label class="label {className ?? ''}">
    {#if label}
        <span>{label}</span>
    {/if}
    <input
        {disabled}
        use:popup={popupSettings}
        class="input"
        type="text"
        bind:value={textValue}
    />
</label>
<select
    data-popup={popupTargetName}
    size={selectSize}
    class="select max-w-fit"
    bind:value
>
    {#if optional}
        <option value={undefined}>(none)</option>
    {/if}
    {#each filteredOptions as option}
        <option value={option.value}>{option.display}</option>
    {/each}
</select>
