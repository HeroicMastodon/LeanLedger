<script lang="ts">

import FormButton from "$lib/components/dialog/FormButton.svelte";
import Alert from "$lib/components/Alert.svelte";
import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
import {ProgressBar} from "@skeletonlabs/skeleton";
import {todaysDateString, firstOfThisYear} from "$lib/dateTools";
import {faRunning} from "@fortawesome/free-solid-svg-icons/faRunning";

let {run, countPromise, text}: {
    run: (start: string, end: string) => void;
    text: string;
    countPromise: Promise<number | undefined>
} = $props();

let startDate = $state(firstOfThisYear());
let endDate = $state(todaysDateString());
</script>

<FormButton
    class="text-warning-500 p-0"
    text={text}
    confirmText="Run"
    onConfirm={() => {run(startDate, endDate); return false;}}
    icon={faRunning}
>
    <div class="flex gap-8">
        <LabeledInput
            type="date"
            bind:value={startDate}
            label="start"
        />
        <LabeledInput
            type="date"
            bind:value={endDate}
            label="end"
        />
    </div>
    {#await countPromise}
        <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
    {:then count}
        <Alert show={count !== undefined} class="variant-filled-success"><p>{count} transactions were edited by the rule</p></Alert>
    {:catch err}
        <Alert show class="variant-filled-error"><p>{err}</p></Alert>
    {/await}
</FormButton>
