import React, { useEffect, useState } from 'react';

function FetchTopFiveCountries({ onSuccess, onError }) {
    useEffect(() => {
        async function fetchTopFiveCountries() {
            try {
                const response = await fetch('/api/Countries/top5');
                if (!response.ok) {
                    throw new Error('Failed to fetch data');
                }
                const data = await response.json();
                onSuccess(data);
                console.log(data)
            } catch (error) {
                console.error('Error fetching top five countries:', error.message);
                onError('Failed to fetch top five countries');
            }
        }

        fetchTopFiveCountries();
    }, []);

    return null;
}

export default FetchTopFiveCountries;
