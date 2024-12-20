<script lang="ts">
    let {class: className, show = false, children}: { class?: string; children?: any; show?: boolean; } = $props();
</script>

{#if show}
    <div class="alert {className ?? ''}">
        <div class="alert-message">
            {#if children}{@render children()}{/if}
        </div>
    </div>
{/if}
