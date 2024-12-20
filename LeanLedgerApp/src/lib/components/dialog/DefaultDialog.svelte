<script lang="ts">
    let { dialog = $bindable<HTMLDialogElement>(), children} = $props();
</script>
<dialog
    bind:this={dialog}
    class="card p-8 mt-48 ml-auto mr-auto text-on-surface-token backdrop:bg-surface-50 backdrop:opacity-10"
    onclick={(event) => { if(event.target === event.currentTarget) dialog?.close() }}
>
    {@render children()}
</dialog>
