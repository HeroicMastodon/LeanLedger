<script lang="ts">
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import Card from "$lib/components/Card.svelte";
    import type { PiggyBank } from "$lib/piggybanks";

    let { piggy = $bindable() }: { piggy: PiggyBank } = $props();
</script>

<Card class="grid grid-cols-1 md:grid-cols-6 gap-4 max-w-3xl items-center">
    <LabeledInput
        class="md:col-span-3"
        label="Name"
        type="text"
        bind:value={piggy.name}
        placeholder="Vacation Fund"
    />

    <MoneyInput
        class="md:col-span-3"
        label="Target"
        bind:value={piggy.targetBalance}
    />
<!-- TODO: Make this day month only or set to the first of the month every time -->
    <LabeledInput
        class="md:col-span-3"
        label="Opened"
        type="date"
        bind:value={piggy.openDate} />

    <LabeledInput
        class="md:col-span-3"
        label="Closed"
        type="date"
        bind:value={piggy.closeDate} />
</Card>
