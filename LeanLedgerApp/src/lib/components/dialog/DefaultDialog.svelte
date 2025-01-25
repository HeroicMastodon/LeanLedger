<script lang="ts">
    import type {MaybePromise} from "$lib";

    let { dialog = $bindable(), children, onenter}:{
        dialog: HTMLDialogElement | undefined,
        children: any,
        onenter?: () => MaybePromise<any>,
    } = $props();

    async function handleKeypress(e: KeyboardEvent & { currentTarget: (EventTarget & HTMLDialogElement) }) {
        if (onenter && e.key === "Enter" && (e.target as HTMLElement)?.nodeName !== "TEXTAREA" && !e.shiftKey) {
            await onenter();
        }
    }
</script>
<dialog
    bind:this={dialog}
    class="card p-8 mt-48 ml-auto mr-auto text-on-surface-token backdrop:bg-surface-50 backdrop:opacity-10 top-[-10rem] md:top-0"
    onclick={(event) => { if(event.target === event.currentTarget) dialog?.close() }}
    onkeyup={handleKeypress}
>
    {@render children()}
</dialog>
