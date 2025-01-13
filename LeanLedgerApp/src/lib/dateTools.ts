export function todaysDateString() {
    return new Date().toLocaleDateString('en-CA', {
        year: 'numeric',
        month: 'numeric',
        day: 'numeric'
    })
}

export function firstOfThisYear() {
    const now = new Date();
    const year = now.getFullYear();
    const firstOfYear = new Date(year, 0, 1); // January is month 0
    return firstOfYear.toISOString().split('T')[0]; // Extracts yyyy-MM-dd
}
