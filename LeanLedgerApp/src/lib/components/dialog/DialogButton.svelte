<script lang="ts">

    import DefaultDialog from "$lib/components/dialog/DefaultDialog.svelte";
    import type {MaybePromise} from "$lib";
    import {Dialog} from "$lib/dialog.svelte";
    import {Fa} from "svelte-fa";
    import type {IconDefinition} from "@fortawesome/free-solid-svg-icons";

    type ColorType = 'primary' | 'error' | 'warning' | 'success';

    let props: {
        text: string;
        children?: any;
        title: string;
        onConfirm: () => MaybePromise<boolean>;
        confirmText: string;
        class?: string;
        confirmButtonColorType?: ColorType;
        cancelButtonColorType?: ColorType;
        cancelText?: string;
        onclick?: () => MaybePromise<any>;
        oncancel?: () => MaybePromise<any>;
        icon?: IconDefinition;
    } = $props();
    let dialog = new Dialog();

    const onConfirm = async () => await dialog.close(props.onConfirm);
    const onClick = async () => await dialog.open(props.onclick);

    async function cancel() {
        await props.oncancel?.();
        return true;
    }

    const close = async () => await dialog.close(cancel);
</script>

<button
    onclick={onClick}
    class="btn {props.class ?? ''}"
    class:btn-icon={!!props.icon}
>
    {#if props.icon}
        <Fa icon={props.icon} />
    {:else}
        {props.text}
    {/if}
</button>
<DefaultDialog bind:dialog={dialog.value} onenter={props.onConfirm}>
    <div class="flex flex-col gap-4">
        <h2 class="h2">{props.title}</h2>
        {#if props.children}
            {@render props.children()}
        {/if}
        <div class="flex gap-4 justify-end">
            <button
                onclick={close}
                class="btn variant-outline-{props.cancelButtonColorType}"
            >{props.cancelText ?? "Cancel"}
            </button>
            <button
                onclick={onConfirm}
                class="btn variant-filled-{props.confirmButtonColorType}"
                type="submit"
            >
                {props.confirmText}
            </button>
        </div>
    </div>
</DefaultDialog>
