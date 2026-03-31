<script lang="ts">
    import { apiClient } from "$lib/apiClient";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import Money from "$lib/components/Money.svelte";
    import PiggyForm from "$lib/piggybanks/PiggyForm.svelte";
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import { faPlus } from "@fortawesome/free-solid-svg-icons/faPlus";
    import { ProgressBar } from "@skeletonlabs/skeleton";

    type Piggy = {
        id: string;
        name: string;
        initialBalance: number;
        balanceTarget: number | null;
        balance: number;
        progressPercent: number | null;
        closed: boolean;
    };

    let piggies: Piggy[] = $state([]);
    let newPiggy = $state(defaultPiggy());

    function defaultPiggy() {
        return {
            id: "",
            name: "",
            initialBalance: 0,
            balanceTarget: undefined,
        };
    }

    async function load() {
        const resp = await apiClient.get<Piggy[]>(
            `piggy-banks?${monthManager.params}`,
        );
        piggies = resp.data;
    }

    async function saveNewPiggy() {
        await apiClient.post("piggy-banks", newPiggy);
        newPiggy = defaultPiggy();
        await load();
        return true;
    }
</script>

<div class="mb-8 flex gap-4 items-center">
    <h1 class="h1">Piggy Banks</h1>
    <FormButton
        class="btn-icon-sm p-2 variant-outline-primary text-primary-500"
        text="New Piggy Bank"
        onConfirm={saveNewPiggy}
        confirmText="Create"
        icon={faPlus}
    >
        <PiggyForm bind:piggy={newPiggy} />
    </FormButton>
</div>

{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <div class="table-container">
        <table class="table table-compact table-hover md:table-fixed">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Balance</th>
                    <th>Target</th>
                    <th>Progress</th>
                </tr>
            </thead>
            <tbody>
                {#each piggies as piggy}
                    <tr>
                        <td
                            ><a
                                class="text-primary-400"
                                href="/piggy-banks/{piggy.id}">{piggy.name}</a
                            ></td
                        >
                        <td><Money amount={piggy.balance} /></td>
                        <td>
                            {#if piggy.balanceTarget != null}
                                <Money amount={piggy.balanceTarget} />
                            {:else}
                                -
                            {/if}
                        </td>
                        <td
                            class="text-left"
                            class:text-error-500={(piggy.progressPercent ?? 0) < 40}
                            class:text-warning-500={(piggy.progressPercent ?? 0) <
                                90 && (piggy.progressPercent ?? 0) >= 40}
                            class:text-success-500={(piggy.progressPercent ?? 0) >=
                                90}
                            >{piggy.progressPercent
                                ? `${piggy.progressPercent.toFixed(1)}%`
                                : "-"}</td
                        >
                    </tr>
                {/each}
            </tbody>
        </table>
    </div>
{:catch err}
    <div class="p-4 variant-filled-error">
        <h3 class="h3">Could not load piggy banks</h3>
        <p class="p">{String(err)}</p>
    </div>
{/await}
