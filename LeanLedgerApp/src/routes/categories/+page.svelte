<script lang="ts">
    import Money from "$lib/components/Money.svelte";
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";

    type Category = { amount: number; name: number; }

    let categories: Category[] = $state([]);

    async function load() {
        const resp = await apiClient.get<Category[]>("categories");
        categories = resp.data;
    }
</script>

<h1 class="h1 mb-8"> Categories </h1>

{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <div class="table-container max-w-lg">
        <table class="table">
            <thead>
            <tr>
                <th>Name</th>
                <th>Amount</th>
            </tr>
            </thead>
            <tbody>
            {#each categories as {name, amount}}
                <tr>
                    <td>
                        <a
                            href="/categories/{name}"
                            class="text-primary-400"
                        >
                            {name}
                        </a>
                    </td>
                    <td>
                        <Money amount={amount} />
                    </td>
                </tr>
            {/each}
            </tbody>
        </table>
    </div>
{/await}





