<script lang="ts">

    import DialogButton from "$lib/components/dialog/DialogButton.svelte";
    import Alert from "$lib/components/Alert.svelte";
    import type {MaybePromise} from "$lib";
    import type {IconDefinition} from "@fortawesome/free-solid-svg-icons";

    let {children, onConfirm, error = $bindable(), text, confirmText, onClick, class:className = "variant-filled-primary", icon}: {
        onConfirm: () => MaybePromise<boolean>;
        children: any,
        text: string;
        error?: string;
        confirmText?: string;
        onClick?: () => MaybePromise<any>,
        class?: string;
        icon?: IconDefinition;
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
    icon={icon}
>
    {@render children()}
    <Alert show={!!error} class="variant-filled-error">
        <p>{error}</p>
    </Alert>
</DialogButton>
