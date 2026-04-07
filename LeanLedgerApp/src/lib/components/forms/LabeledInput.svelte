<script lang="ts">
    type InputType = "text" | "number" | "date";

    let {type, value = $bindable(), class: className, label, placeholder, onkeyup, disabled}: {
        label?: string;
        type: InputType;
        value?: string | number;
        class?: string;
        placeholder?: string;
        disabled?: boolean;
        onkeyup?: (value: Event & { currentTarget: EventTarget & HTMLInputElement }) => void;
    } = $props();
</script>

<label class="label {className ?? ''}">
    {#if label}
        <span>{label}</span>
    {/if}
    <input type={type} class="input" bind:value={value} placeholder={placeholder} {onkeyup} {disabled} />
</label>
