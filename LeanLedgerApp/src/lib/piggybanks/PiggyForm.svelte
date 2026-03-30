<script lang="ts">
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import Card from "$lib/components/Card.svelte";
    import type {PiggyData} from "$lib/piggybanks";

let {piggy = $bindable()}: { piggy: PiggyData } = $props();

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
        class="md:col-span-2"
        label="Initial Balance"
        bind:value={piggy.initialBalance}
    />

    <MoneyInput
        class="md:col-span-2"
        label="Target"
        bind:value={piggy.balanceTarget}
    />
</Card>
