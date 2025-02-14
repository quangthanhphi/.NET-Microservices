import EmptyFilter from '@/app/components/EmptyFilter'
import React from 'react'

export default function SignIn({searchParams}: {searchParams: {callbackUrl: string}}) {
  return (
    <EmptyFilter 
      title='You need to sign in to access this page'
      subtitle='Please click below to sign in'
      showLogin
      callbackUrl={searchParams.callbackUrl}
    />
  )
}
