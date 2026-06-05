import { profileIcons } from '@/utils/profileIcons'

export const getProfileIcon = (iconId) => {
  if (!iconId) return ''

  const todosLosIconos = [
    ...profileIcons.admin,
    ...profileIcons.doctor,
    ...profileIcons.paciente
  ]

  const iconoEncontrado = todosLosIconos.find(
    icon => icon.id === iconId
  )

  return iconoEncontrado?.src || ''
}
