{#snippet errorMessage(err: any)}
    <div class="alert variant-filled-error">
        <div class="alert-message">
            <h3 class="h3">Something went wrong</h3>
            <p>We couldn't load the account. Please try again {!!err ? err : ""}</p>
        </div>
    </div>
{/snippet}
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    {#if !account}
        {@render errorMessage(null)}
    {:else }
        <div class="mb-4 flex gap-4 justify-start items-center">
            <h1 class="h1">Account</h1>
            <button class="btn variant-filled-primary" onclick={ saveChanges }>Save</button>
            <button onclick={showDeleteConfirmation} class="btn variant-outline-error">Delete</button>
            <dialog
                onclick={ (event) => {if (event.target === event.currentTarget) confirmationDialog?.close() }}
                bind:this={confirmationDialog}
                class="card p-8 mt-48 ml-auto mr-auto  text-on-surface-token"
            >
                <div class="flex flex-col gap-4">
                    <h2 class="h2">Are you sure you want to delete?</h2>
                    <div class="flex gap-4">
                        <button onclick={() => confirmationDialog?.close()} class="btn variant-outline-success">Cancel
                        </button>
                        <button onclick={deleteAccount} class="btn variant-filled-error">Delete</button>
                    </div>
                </div>
            </dialog>
            <p>Balance:
                <Money amount={account.balance} />
            </p>
            {#if isSaving}
                <ProgressRadial width="w-5" meter="stroke-primary-500" track="strock-primary-500/30" />
            {/if}
        </div>
        <AccountForm bind:account={account} />
        <!--TODO: Add metrics-->
        <h2 class="h2 mt-8 mb-4">Transactions</h2>
        <div class="table-container">
            <table class="table">
                <thead>
                <tr>
                    <th>Description</th>
                    <th>Amount</th>
                    <th>Date</th>
                    <th>Source Account</th>
                    <th>Destination Account</th>
                    <th>Category</th>
                </tr>
                </thead>
                <tbody>
                {#each account.transactions as transaction}
                    <tr>
                        <td>{transaction.description}</td>
                        <td>${transaction.amount}</td>
                        <td>{transaction.date}</td>
                        <td>{transaction.sourceAccount?.name ?? ""}</td>
                        <td>{transaction.destinationAccount?.name ?? ""}</td>
                        <td>{transaction.category ?? ""}</td>
                    </tr>
                {/each}
                </tbody>
            </table>
        </div>
    {/if}

{:catch err}
    {@render errorMessage(err)}
{/await}
<script lang="ts">
    import type {PageData} from "./$types";
    import {Fa} from "svelte-fa";
    import {faDollar} from "@fortawesome/free-solid-svg-icons";
    import {apiClient} from "$lib/apiClient";
    import {type AccountData, accountTypes} from "$lib/accounts";
    import {ProgressBar, ProgressRadial} from "@skeletonlabs/skeleton";
    import AccountForm from "$lib/accounts/AccountForm.svelte";
    import Money from "$lib/components/Money.svelte";
    import {goto} from "$app/navigation"

    let {data}: { data: PageData } = $props();
    let account: AccountData | null = $state(null);
    let isSaving = $state(false);

    async function load() {
        const response = await apiClient.get<AccountData>(`accounts/${data.id}`);
        account = response.data;
        console.dir(response.data)
    }

    async function saveChanges() {
        isSaving = true;
        const response = await apiClient.put(`accounts/${data.id}`, account)
        console.dir(response)
        isSaving = false;
    }

    let confirmationDialog: HTMLDialogElement | undefined = $state()

    function showDeleteConfirmation() {
        confirmationDialog?.showModal()
    }

    async function deleteAccount() {
        const response = await apiClient.delete(`accounts/${data.id}`);
        await goto("/accounts")
    }
</script>
