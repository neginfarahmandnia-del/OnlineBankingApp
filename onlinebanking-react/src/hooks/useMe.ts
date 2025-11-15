// src/hooks/useMe.ts
import { useEffect, useState } from "react";
import { useAuth } from "../store/auth";

export type MeResponse = {
    userId?: string;
    email?: string;
    roles?: string[];
};

export function useMe(
    enabled = true
): {
    me: MeResponse | null;
    loading: boolean;
    error: any;
} {
    const token = useAuth((s) => s.token);
    const meFn = useAuth((s) => s.me);

    const [me, setMe] = useState<MeResponse | null>(null);
    const [loading, setLoading] = useState(enabled);
    const [error, setError] = useState<any>(null);

    useEffect(() => {
        if (!enabled || !token) {
            setLoading(false);
            setMe(null);
            return;
        }

        let cancelled = false;
        setLoading(true);

        meFn()
            .then((data) => {
                if (!cancelled) setMe(data);
            })
            .catch((err) => {
                if (!cancelled) setError(err);
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });

        return () => {
            cancelled = true;
        };
    }, [enabled, token, meFn]);

    return { me, loading, error };
}
