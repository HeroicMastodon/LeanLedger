<script lang="ts">
    import {apiClient} from "$lib/apiClient";
    import {onMount} from "svelte";
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import {goto} from "$app/navigation";

    let {transactionId = $bindable()}: { transactionId: string } = $props();

    type Allocation = { id: string; amount: number; piggyBankId: string; piggyName: string };
    type PiggyOption = { id: string; name: string };

    let allocations: Allocation[] = $state([]);
    let piggies: PiggyOption[] = $state([]);

    let newPiggyId: string = $state("");
    let newAmount: number = $state(0);
    let loading = $state(true);

    // Transaction info (loaded so we can do client-side validation)
    let transactionAmount: number = $state(0);
    let transactionType: string = $state("");
    let newAllocationError: string = $state("");

    async function load() {
        loading = true;
        const [aRes, pRes, tRes] = await Promise.all([
            apiClient.get<Allocation[]>(`transactions/${transactionId}/allocations`),
            apiClient.get<any[]>(`piggy-banks`),
            apiClient.get<any>(`transactions/${transactionId}`),
        ]);

        allocations = aRes.data;
        piggies = pRes.data.map(p => ({ id: p.id, name: p.name }));
        transactionAmount = tRes.data?.amount ?? 0;
        transactionType = tRes.data?.type ?? "";
        newAllocationError = "";
        loading = false;
    }

    onMount(load);

    function allocatedSum(): number {
        return allocations.reduce((s, a) => s + (a.amount ?? 0), 0);
    }

    async function createAllocation() {
        newAllocationError = "";
        if (!newPiggyId) {
            newAllocationError = "Select a piggy bank.";
            return;
        }
        if (!newAmount || newAmount <= 0) {
            newAllocationError = "Amount must be greater than zero.";
            return;
        }
        if (transactionType === 'Transfer') {
            newAllocationError = "Cannot add allocations to Transfer transactions.";
            return;
        }

        const remaining = transactionAmount - allocatedSum();
        if (newAmount > remaining) {
            newAllocationError = `Allocation exceeds remaining amount (${remaining}).`;
            return;
        }

        await apiClient.post(`transactions/${transactionId}/allocations`, { piggyBankId: newPiggyId, amount: newAmount });
        newPiggyId = "";
        newAmount = 0;
        await load();
    }

    async function deleteAllocation(id: string) {
        await apiClient.delete(`transactions/${transactionId}/allocations/${id}`);
        await load();
    }

    let editId: string | null = $state(null);
    let editPiggyId: string = $state("");
    let editAmount: number = $state(0);

    function beginEdit(a: Allocation) {
        editId = a.id;
        editPiggyId = a.piggyBankId;
        editAmount = a.amount;
    }

    async function saveEdit() {
        if (!editId) return;
        // client-side check: ensure edited amount doesn't push total > transactionAmount
        const otherSum = allocations.filter(a => a.id !== editId).reduce((s, a) => s + (a.amount ?? 0), 0);
        if (transactionType === 'Transfer') return;
        if (editAmount <= 0) return;
        if (otherSum + editAmount > transactionAmount) return;

        await apiClient.put(`transactions/${transactionId}/allocations/${editId}`, { piggyBankId: editPiggyId, amount: editAmount });
        editId = null;
        await load();
    }
</script>

{#if loading}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:else}
    <h2 class="h2 mt-8 mb-4">Allocations</h2>

    <div class="table-container">
        <table class="table table-compact table-hover md:table-fixed w-full">
            <thead>
                <tr>
                    <th>Piggy</th>
                    <th>Amount</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                {#each allocations as a}
                    <tr>
                        <td>
                            <a class="text-primary-400" href="/piggy-banks/{a.piggyBankId}">{a.piggyName}</a>
                        </td>
                        <td>
                            {#if editId === a.id}
                                <MoneyInput bind:value={editAmount} />
                            {:else}
                                <span>{a.amount}</span>
                            {/if}
                        </td>
                        <td class="flex gap-2">
                            {#if editId === a.id}
                                <select bind:value={editPiggyId} class="select">
                                    {#each piggies as p}
                                        <option value={p.id}>{p.name}</option>
                                    {/each}
                                </select>
                                <button class="btn text-success-500" onclick={saveEdit}>Save</button>
                                <button class="btn text-warning-500" onclick={() => (editId = null)}>Cancel</button>
                            {:else}
                                <button class="btn text-primary-500" onclick={() => beginEdit(a)}>Edit</button>
                                <button class="btn text-error-500" onclick={() => deleteAllocation(a.id)}>Delete</button>
                            {/if}
                        </td>
                    </tr>
                {/each}
            </tbody>
        </table>
    </div>

    <div class="mt-4 grid grid-cols-1 md:grid-cols-6 gap-4 items-end">
        <select class="select md:col-span-3" bind:value={newPiggyId}>
            <option value="">Select piggy...</option>
            {#each piggies as p}
                <option value={p.id}>{p.name}</option>
            {/each}
        </select>
        <MoneyInput class="md:col-span-2" bind:value={newAmount} />
        <button class="btn text-success-500 md:col-span-1" onclick={createAllocation}>Add</button>
    </div>
{/if}
