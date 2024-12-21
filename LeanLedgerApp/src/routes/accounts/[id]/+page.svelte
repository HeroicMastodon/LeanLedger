<script lang="ts">
    import type {PageData} from "./$types";
    import {apiClient} from "$lib/apiClient";
    import {type AccountData} from "$lib/accounts";
    import {ProgressBar, ProgressRadial} from "@skeletonlabs/skeleton";
    import AccountForm from "$lib/accounts/AccountForm.svelte";
    import Money from "$lib/components/Money.svelte";
    import {goto} from "$app/navigation"
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";
    import DeleteConfirmationButton from "$lib/components/dialog/DeleteConfirmationButton.svelte";
    import Alert from "$lib/components/Alert.svelte";
    import ImportSettingsButton from "$lib/accounts/ImportSettingsButton.svelte";

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

    async function deleteAccount() {
        const response = await apiClient.delete(`accounts/${data.id}`);
        await goto("/accounts")

        return false;
    }
</script>

{#snippet errorMessage(err: any)}
    <Alert show class="variant-filled-error">
        <h3 class="h3">Something went wrong</h3>
        <p>We couldn't load the account. Please try again {!!err ? err : ""}</p>
    </Alert>
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
            {#if account.accountType !== "Merchant"}
                <ImportSettingsButton accountId={account.id} />
            {/if}
            <DeleteConfirmationButton onDelete={deleteAccount} />
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
        <TransactionTable transactions={account.transactions} />
    {/if}

{:catch err}
    {@render errorMessage(err)}
{/await}

