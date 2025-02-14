'use client'

import { useParamsStore } from '@/hooks/useParamsStore';
import { usePathname } from 'next/navigation';
import { useRouter } from 'next/navigation';
import React from 'react'
import { FaCar } from "react-icons/fa";

export default function Logo() {
  const router = useRouter();
  const pathName = usePathname();

  function doReset() {
    if (pathName !== '/') router.push('/');
    reset();
  }

  const reset = useParamsStore(state => state.reset);
  return (
    <div>
      <div onClick={doReset} className="flex cursor-pointer items-center gap-2 text-3xl font-semibold text-red-500">
        <FaCar size={34} />
        <div>Car Auctions</div>
      </div>
    </div>
  )
}
