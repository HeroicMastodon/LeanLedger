<script lang="ts">

import DialogButton from "$lib/components/dialog/DialogButton.svelte";
import type {MaybePromise} from "$lib";
import {faTrashCan} from "@fortawesome/free-solid-svg-icons/faTrashCan";
let {onDelete}: {onDelete: () => MaybePromise<boolean>;} = $props();
</script>

<DialogButton
    title="Are you sure you want to delete?"
    confirmText="Delete"
    onConfirm={onDelete}
    text="Delete"
    class="text-error-500 p-2"
    confirmButtonColorType="error"
    cancelButtonColorType="success"
    icon={faTrashCan}
/>
