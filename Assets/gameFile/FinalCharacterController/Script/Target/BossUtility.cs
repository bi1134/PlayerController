public static class BossUtility
{
    public static bool ShouldTeleport(Enemy enemy)
    {
        return enemy.IsBoss &&
               enemy.config.behaviorProfile.canDash &&
               enemy.config.behaviorProfile.dashIsTeleport &&
               enemy.teleportCooldownTimer <= 0f;
    }
}
