<script lang="ts">

    import DialogButton from "$lib/components/dialog/DialogButton.svelte";
    import Alert from "$lib/components/Alert.svelte";
    import type {MaybePromise} from "$lib";

    let {children, onConfirm, error = $bindable(), text, confirmText, onClick, class:className = "variant-filled-primary"}: {
        onConfirm: () => MaybePromise<boolean>;
        children: any,
        text: string;
        error?: string;
        confirmText?: string;
        onClick?: () => MaybePromise<any>,
        class?: string;
    } = $props();
</script>

<DialogButton
    class={className}
    title={text}
    text={text}
    onConfirm={onConfirm}
    confirmText={confirmText ?? "Save"}
    confirmButtonColorType="success"
    cancelButtonColorType="error"
    onclick={onClick}
    onenter={onConfirm}
>
    {@render children()}
    <Alert show={!!error} class="variant-filled-error">
        <p>{error}</p>
    </Alert>
</DialogButton>
