<script lang="ts">
    let {
        value = $bindable(),
        label,
        class: className,
        options,
        inputId,
        datalistId,
    }: {
        value?: string;
        label: string;
        class?: string;
        options: string[];
        inputId: string;
        datalistId: string;
    } = $props();

</script>

<label class="label {className ?? ''}">
    <span>{label}</span>
    <input list={datalistId} id={inputId} type="text" class="input">
    <datalist id={datalistId}>
        {#each options as option}
            <option value={ option }>{option}</option>
        {/each}
    </datalist>
</label>
