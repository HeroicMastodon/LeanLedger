<script lang="ts">
    import {Fa} from "svelte-fa";
    import {faDollar} from "@fortawesome/free-solid-svg-icons";

    let {value = $bindable(), class: className, label, disabled}: { value?: number | string; class?: string; label?: string; disabled?: boolean; } = $props();
</script>
<div class="label {className ?? ''}">
    {#if label}
        <span>{label}</span>
    {/if}
    <div class="input-group input-group-divider grid-cols-[auto_1fr_auto]">
        <div class="input-group-shim">
            <Fa icon={faDollar}></Fa>
        </div>
        <input disabled={disabled} type="number" bind:value={value} />
    </div>
</div>
