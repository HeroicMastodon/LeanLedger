<script lang="ts">
    const props = $props<{class?: string, children?: any}>();
</script>

<div class="card p-4 {props.class ?? ''}">
    {#if props.children}
        {@render props.children()}
    {/if}
</div>
