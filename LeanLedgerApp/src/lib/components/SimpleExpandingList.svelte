<script lang="ts" generics="TItem">
    let {items, stringify, label}: { items: TItem[], stringify: (item: TItem) => string; label: string; } = $props();
    let expand = $state(false);
</script>


<ul class="list {items.length > 1 ? 'hover:cursor-pointer' : ''}" onclick={() => expand = !expand}>
    {#if items.length === 0}
        <li>No {label}</li>
    {:else if items.length > 1 && !expand}
        <li>Show {label}</li>
    {:else}
        {#each items as item}
            <li>{stringify(item)}</li>
        {/each}
    {/if}
</ul>
