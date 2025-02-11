'use server'

import { Auction, PagedResult } from "@/types";

export async function getData(query: string): Promise<PagedResult<Auction>> {
  const res = await fetch(`http://localhost:6001/search${query}`, {
    next: { revalidate: 3600 }, // Cache 1 giờ
  });

  if (!res.ok) throw new Error("Failed to fetch data");

  return res.json();
}