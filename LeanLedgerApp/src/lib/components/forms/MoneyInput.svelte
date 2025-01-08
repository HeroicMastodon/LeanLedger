<script lang="ts">
    import {Fa} from "svelte-fa";
    import {faDollar} from "@fortawesome/free-solid-svg-icons";

    let {value = $bindable(), class: className, label}: { value: number; class?: string; label: string; } = $props();
</script>
<div class="label {className ?? ''}">
    <span>{label}</span>
    <div class="input-group input-group-divider grid-cols-[auto_1fr_auto]">
        <div class="input-group-shim">
            <Fa icon={faDollar}></Fa>
        </div>
        <input type="number" bind:value={value} />
    </div>
</div>
