<script lang="ts">
    import Money from "$lib/components/Money.svelte";

    let { difference }: { difference: number } = $props();
</script>

{#if difference > 0}
    <div class="text-warning-500">
        Need to allocate: <Money amount={difference} />
    </div>
{:else if difference < 0}
    <div class="text-error-500">
        Need to disburse: <Money amount={-difference} />
    </div>
{:else}
    <div class="text-success-500">All piggies are perfectly allocated!</div>
{/if}
