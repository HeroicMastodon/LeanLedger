<script lang="ts">
    type InputType = "text" | "number" | "date";

    let {type, value = $bindable(), class: className, label, placeholder}: {
        label?: string;
        type: InputType;
        value: string | number;
        class?: string;
        placeholder?: string;
    } = $props();
</script>

<label class="label {className ?? ''}">
    {#if label}
        <span>{label}</span>
    {/if}
    <input type={type} class="input" bind:value={value} placeholder={placeholder} />
</label>
