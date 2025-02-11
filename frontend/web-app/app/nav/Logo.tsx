import { useParamsStore } from '@/hooks/useParamsStore';
import React from 'react'
import { FaCar } from "react-icons/fa";

export default function Logo() {
  const reset = useParamsStore(state => state.reset);
  return (
    <div>
      <div onClick={reset} className="flex cursor-pointer items-center gap-2 text-3xl font-semibold text-red-500">
        <FaCar size={34} />
        <div>Car Auctions</div>
      </div>
    </div>
  )
}
