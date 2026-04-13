<script lang="ts">
    import { apiClient } from "$lib/apiClient";
    import { type AccountData } from "$lib/accounts";
    import { ProgressBar, ProgressRadial } from "@skeletonlabs/skeleton";
    import AccountForm from "$lib/accounts/AccountForm.svelte";
    import Money from "$lib/components/Money.svelte";
    import { goto } from "$app/navigation";
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";
    import DeleteConfirmationButton from "$lib/components/dialog/DeleteConfirmationButton.svelte";
    import Alert from "$lib/components/Alert.svelte";
    import ImportSettingsButton from "$lib/accounts/ImportSettingsButton.svelte";
    import ImportButton from "$lib/accounts/ImportButton.svelte";
    import { page } from "$app/stores";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import { Fa } from "svelte-fa";
    import { faSave } from "@fortawesome/free-solid-svg-icons/faSave";
    import { sumUp } from "$lib";

    let id = $page.params.id;
    let account = $state<AccountData>();
    let isSaving = $state(false);

    async function load() {
        const response = await apiClient.get<AccountData>(
            `accounts/${id}?${monthManager.params}`,
        );
        account = response.data;
    }

    async function saveChanges() {
        isSaving = true;
        const response = await apiClient.put(`accounts/${id}`, account);
        isSaving = false;
    }

    async function deleteAccount() {
        const response = await apiClient.delete(`accounts/${id}`);
        await goto("/accounts");

        return false;
    }

    const totalIncome = $derived(
        sumUp(
            account?.transactions.filter(
                (t) =>
                    t.type == "Income" ||
                    (t.type == "Transfer" &&
                        t.destinationAccount?.id == account?.id),
            ) ?? [],
            (t) => t.amount,
        ),
    );
    const totalExpenses = $derived(
        sumUp(
            account?.transactions.filter(
                (t) =>
                    t.type == "Expense" ||
                    (t.type == "Transfer" &&
                        t.sourceAccount?.id == account?.id),
            ) ?? [],
            (t) => t.amount,
        ),
    );
    const totalChange = $derived(totalIncome - totalExpenses);
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
    {:else}
        <div class="mb-4 flex gap-4 justify-start items-center flex-wrap">
            <h1 class="h1">Account</h1>
            <button class="btn text-primary-500 p-2" onclick={saveChanges}>
                <Fa icon={faSave} />
            </button>
            {#if account.accountType !== "Merchant"}
                <ImportSettingsButton accountId={account.id} />
                <ImportButton onClose={load} accountId={account.id} />
            {/if}
            <DeleteConfirmationButton onDelete={deleteAccount} />
            <p>
                Balance:
                <Money amount={account.balance} />
            </p>
            {#if isSaving}
                <ProgressRadial
                    width="w-5"
                    meter="stroke-primary-500"
                    track="strock-primary-500/30"
                />
            {/if}
        </div>
        <AccountForm bind:account />
        <!--TODO: Add metrics-->
        <div class="flex gap-4 items-center mt-8 mb-4">
            <h2 class="h2">Transactions</h2>
            <div>
                In: <Money amount={totalIncome} />
            </div>
            <div>
                Out: <Money type="Expense" amount={totalExpenses} />
            </div>
            <div>
                Change: <Money amount={totalChange} />
            </div>
        </div>
        <TransactionTable transactions={account.transactions} />
    {/if}
{:catch err}
    {@render errorMessage(err)}
{/await}
