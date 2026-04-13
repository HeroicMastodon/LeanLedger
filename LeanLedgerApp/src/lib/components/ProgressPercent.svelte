<script lang="ts">
    import { exists } from "$lib";

    let { progress }: { progress?: number } = $props();
</script>

<span
    class:text-error-400={progress && progress  < 40}
    class:text-warning-400={progress && progress < 90 && progress >= 40}
    class:text-success-500={progress && progress >= 90}
>
    {exists(progress) ? `${progress!.toFixed(1)}%` : "-"}
</span>
